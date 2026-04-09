using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace ScientificActivityParsers.Parsers
{
    public class VakPdfJournalParser : IJournalParser
    {
        private static readonly Regex JournalNumberRegex =
            new(@"^\d+\.$", RegexOptions.Compiled);

        private static readonly Regex IssnRegex =
            new(@"\b\d{4}-\d{3}[\dXХxх]\b", RegexOptions.Compiled);

        private static readonly Regex ModernSpecialtyRegex =
            new(
                @"(?<code>\d+\.\d+\.\d+)\.\s*(?<name>.*?)(?<branch>\((?:[^()]|\([^()]*\))*\))(?=(\s+\d+\.\d+\.\d+\.\s)|$)",
                RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex OldSpecialtyRegex =
            new(
                @"(?<code>\d{2}\.\d{2}\.\d{2})\s*[–-]\s*(?<name>.*?)(?<branch>\((?:[^()]|\([^()]*\))*\))(?=(\s+\d{2}\.\d{2}\.\d{2}\s*[–-])|$)",
                RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex DateFromRegex =
            new(
                @"(?:\bс\s*)?(\d{2}(?:\.\d{2}\.\d{4}|\.\d{6}|\d{6}|\.\d{2}\.\d{3}|\.\d{5}|\d{7}))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DateToRegex =
            new(
                @"по\s*(\d{2}(?:\.\d{2}\.\d{4}|\.\d{6}|\d{6}|\.\d{2}\.\d{3}|\.\d{5}|\d{7}))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Task<List<JournalImportModel>> ParseVakPdfAsync(
            string pdfPath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                throw new ArgumentNullException(nameof(pdfPath));
            }

            var rawRows = ReadRows(pdfPath, cancellationToken);

            var journals = rawRows
                .Select(ParseJournal)
                .Where(x => x != null)
                .Cast<JournalImportModel>()
                .GroupBy(BuildJournalKey)
                .Select(MergeDuplicateJournals)
                .ToList();

            return Task.FromResult(journals);
        }

        private static List<RawVakJournalRow> ReadRows(string pdfPath, CancellationToken cancellationToken)
        {
            using var document = PdfDocument.Open(pdfPath);

            var result = new List<RawVakJournalRow>();
            RawVakJournalRow? current = null;

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToList();
                var lines = BuildLines(words);

                foreach (var line in lines)
                {
                    if (IsHeaderOrGarbage(line))
                    {
                        continue;
                    }

                    var split = SplitIntoColumns(line, page.Width);

                    if (string.IsNullOrWhiteSpace(split.Number) &&
                        string.IsNullOrWhiteSpace(split.Title) &&
                        string.IsNullOrWhiteSpace(split.Issn) &&
                        string.IsNullOrWhiteSpace(split.Specialties) &&
                        string.IsNullOrWhiteSpace(split.Date))
                    {
                        continue;
                    }

                    if (JournalNumberRegex.IsMatch(split.Number))
                    {
                        if (current != null)
                        {
                            result.Add(current);
                        }

                        current = new RawVakJournalRow
                        {
                            Number = split.Number.Trim().TrimEnd('.')
                        };
                    }

                    if (current == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(split.Title))
                    {
                        current.TitleParts.Add(split.Title);
                    }

                    if (!string.IsNullOrWhiteSpace(split.Issn))
                    {
                        current.IssnParts.Add(split.Issn);
                    }

                    current.SpecialtyRows.Add(new RawVakSpecialtyRow
                    {
                        SpecialtyText = split.Specialties,
                        DateText = split.Date
                    });
                }
            }

            if (current != null)
            {
                result.Add(current);
            }

            return result;
        }

        private static List<RawLine> BuildLines(List<Word> words)
        {
            var ordered = words
                .OrderByDescending(x => x.BoundingBox.Bottom)
                .ThenBy(x => x.BoundingBox.Left)
                .ToList();

            var lines = new List<RawLine>();
            const double tolerance = 2.0;

            foreach (var word in ordered)
            {
                var y = word.BoundingBox.Bottom;
                var line = lines.FirstOrDefault(x => Math.Abs(x.Y - y) <= tolerance);

                if (line == null)
                {
                    line = new RawLine { Y = y };
                    lines.Add(line);
                }

                line.Words.Add(new RawWord
                {
                    Text = word.Text,
                    X = word.BoundingBox.Left
                });
            }

            foreach (var line in lines)
            {
                line.Words = line.Words.OrderBy(x => x.X).ToList();
            }

            return lines
                .OrderByDescending(x => x.Y)
                .ToList();
        }

        private static bool IsHeaderOrGarbage(RawLine line)
        {
            var text = NormalizeText(string.Join(" ", line.Words.Select(x => x.Text)));

            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (Regex.IsMatch(text, @"^\d+$"))
            {
                return true;
            }

            if (text.Contains("ПЕРЕЧЕНЬ", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("рецензируемых научных изданий", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("по состоянию на", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("№ п/п", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Наименование издания", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Научные специальности", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Дата включения", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static SplitLine SplitIntoColumns(RawLine line, double pageWidth)
        {
            var numberBorder = pageWidth * 0.07;
            var titleBorder = pageWidth * 0.39;
            var issnBorder = pageWidth * 0.50;
            var specialtyBorder = pageWidth * 0.86;

            var number = new List<string>();
            var title = new List<string>();
            var issn = new List<string>();
            var specialties = new List<string>();
            var date = new List<string>();

            foreach (var word in line.Words)
            {
                if (word.X < numberBorder)
                {
                    number.Add(word.Text);
                }
                else if (word.X < titleBorder)
                {
                    title.Add(word.Text);
                }
                else if (word.X < issnBorder)
                {
                    issn.Add(word.Text);
                }
                else if (word.X < specialtyBorder)
                {
                    specialties.Add(word.Text);
                }
                else
                {
                    date.Add(word.Text);
                }
            }

            return new SplitLine
            {
                Number = NormalizeText(string.Join(" ", number)),
                Title = NormalizeText(string.Join(" ", title)),
                Issn = NormalizeText(string.Join(" ", issn)),
                Specialties = NormalizeText(string.Join(" ", specialties)),
                Date = NormalizeText(string.Join(" ", date))
            };
        }

        private static JournalImportModel? ParseJournal(RawVakJournalRow row)
        {
            var rawTitle = NormalizeText(string.Join(" ", row.TitleParts));
            var rawIssn = NormalizeText(string.Join(" ", row.IssnParts));

            var titleIssns = IssnRegex.Matches(rawTitle)
                .Select(x => NormalizeIssn(x.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var columnIssns = IssnRegex.Matches(rawIssn)
                .Select(x => NormalizeIssn(x.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var issns = columnIssns
                .Concat(titleIssns)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cleanedTitle = CleanJournalTitle(rawTitle, issns);

            if (string.IsNullOrWhiteSpace(cleanedTitle))
            {
                return null;
            }

            var blocks = BuildSpecialtyBlocks(row.SpecialtyRows);
            var specialties = new List<JournalVakSpecialtyImportModel>();

            foreach (var block in blocks)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(block.SpecialtyText))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(block.DateText))
                    {
                        continue;
                    }

                    var parsedDates = ParseDates(block.DateText);
                    var parsedSpecialties = ParseSpecialties(
                        block.SpecialtyText,
                        parsedDates.dateFrom,
                        parsedDates.dateTo);

                    specialties.AddRange(parsedSpecialties);
                }
                catch
                {
                    continue;
                }
            }

            return new JournalImportModel
            {
                Title = cleanedTitle,
                Issn = issns.ElementAtOrDefault(0),
                EIssn = issns.ElementAtOrDefault(1),
                AllIssns = issns,
                Url = null,
                WhiteListLevel2023 = null,
                WhiteListLevel2025 = null,
                WhiteListState = null,
                WhiteListNotice = null,
                WhiteListAcceptedDate = null,
                WhiteListDiscontinuedDate = null,
                IsVak = true,
                SourceName = "Перечень ВАК",
                SourceActualDate = new DateTime(2026, 2, 17),
                SubjectArea = string.Join("; ",
                    specialties
                        .Select(x => $"{x.SpecialtyCode} {x.SpecialtyName}")
                        .Distinct(StringComparer.OrdinalIgnoreCase)),
                VakSpecialties = specialties
            };
        }

        private static List<SpecialtyBlock> BuildSpecialtyBlocks(List<RawVakSpecialtyRow> rows)
        {
            var result = new List<SpecialtyBlock>();
            SpecialtyBlock? current = null;

            foreach (var row in rows)
            {
                var spec = NormalizeText(row.SpecialtyText);
                var date = NormalizeText(row.DateText);

                var startsNewBlock =
                    !string.IsNullOrWhiteSpace(date) &&
                    (date.StartsWith("с ", StringComparison.OrdinalIgnoreCase) ||
                     date.StartsWith("С ", StringComparison.OrdinalIgnoreCase) ||
                     Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                     Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{3}$") ||
                     Regex.IsMatch(date, @"^\d{2}\.\d{6}$") ||
                     Regex.IsMatch(date, @"^\d{8}$") ||
                     date.Contains("по ", StringComparison.OrdinalIgnoreCase));

                if (startsNewBlock)
                {
                    if (current != null &&
                        (!string.IsNullOrWhiteSpace(current.SpecialtyText) || !string.IsNullOrWhiteSpace(current.DateText)))
                    {
                        result.Add(current);
                    }

                    current = new SpecialtyBlock();
                }

                current ??= new SpecialtyBlock();

                if (!string.IsNullOrWhiteSpace(spec))
                {
                    current.SpecialtyText = string.IsNullOrWhiteSpace(current.SpecialtyText)
                        ? spec
                        : $"{current.SpecialtyText} {spec}";
                }

                if (!string.IsNullOrWhiteSpace(date))
                {
                    current.DateText = string.IsNullOrWhiteSpace(current.DateText)
                        ? date
                        : $"{current.DateText} {date}";
                }
            }

            if (current != null &&
                (!string.IsNullOrWhiteSpace(current.SpecialtyText) || !string.IsNullOrWhiteSpace(current.DateText)))
            {
                result.Add(current);
            }

            return result;
        }

        private static List<JournalVakSpecialtyImportModel> ParseSpecialties(
            string specialtyText,
            DateTime dateFrom,
            DateTime? dateTo)
        {
            var normalized = NormalizeText(specialtyText);
            var result = new List<JournalVakSpecialtyImportModel>();

            var matches = ModernSpecialtyRegex.Matches(normalized).Cast<Match>().ToList();
            if (matches.Count == 0)
            {
                matches = OldSpecialtyRegex.Matches(normalized).Cast<Match>().ToList();
            }

            foreach (var match in matches)
            {
                var code = NormalizeText(match.Groups["code"].Value).TrimEnd('.');
                var name = NormalizeText(match.Groups["name"].Value)
                    .Trim()
                    .TrimEnd(',', ';');
                var branch = NormalizeText(match.Groups["branch"].Value)
                    .Trim()
                    .TrimStart('(')
                    .TrimEnd(')');

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                result.Add(new JournalVakSpecialtyImportModel
                {
                    SpecialtyCode = code,
                    SpecialtyName = name,
                    ScienceBranch = branch,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                });
            }

            return result;
        }

        private static (DateTime dateFrom, DateTime? dateTo) ParseDates(string dateText)
        {
            var normalized = NormalizeDateText(dateText);

            var fromMatch = DateFromRegex.Match(normalized);
            if (!fromMatch.Success)
            {
                throw new InvalidOperationException($"Не удалось определить дату начала из строки: {normalized}");
            }

            var dateFrom = ParseLooseRussianDate(fromMatch.Groups[1].Value);

            DateTime? dateTo = null;
            var toMatch = DateToRegex.Match(normalized);
            if (toMatch.Success)
            {
                dateTo = ParseLooseRussianDate(toMatch.Groups[1].Value);
            }

            return (dateFrom, dateTo);
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("￾", "")
                .Replace("−", "-")
                .Replace("—", "–")
                .Replace("­", "")
                .Replace("\u00A0", " ");

            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = normalized.Replace(" ,", ",");
            normalized = normalized.Replace(" .", ".");
            normalized = normalized.Replace(" ;", ";");
            normalized = normalized.Trim();

            return normalized;
        }

        private static string NormalizeIssn(string value)
        {
            return NormalizeText(value)
                .ToUpperInvariant()
                .Replace('Х', 'X')
                .Replace('х', 'X');
        }

        private static string BuildJournalKey(JournalImportModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                return model.Issn.ToUpperInvariant();
            }

            return model.Title.Trim().ToUpperInvariant();
        }

        private static JournalImportModel MergeDuplicateJournals(IGrouping<string, JournalImportModel> group)
        {
            var first = group.First();

            first.VakSpecialties = group
                .SelectMany(x => x.VakSpecialties)
                .GroupBy(x => $"{x.SpecialtyCode}|{x.SpecialtyName}|{x.ScienceBranch}|{x.DateFrom:yyyy-MM-dd}|{x.DateTo:yyyy-MM-dd}")
                .Select(x => x.First())
                .ToList();

            first.SubjectArea = string.Join("; ",
                first.VakSpecialties
                    .Select(x => $"{x.SpecialtyCode} {x.SpecialtyName}")
                    .Distinct(StringComparer.OrdinalIgnoreCase));

            return first;
        }

        private static DateTime ParseLooseRussianDate(string rawDate)
        {
            var value = NormalizeText(rawDate)
                .Replace(" ", string.Empty)
                .Replace(",", string.Empty)
                .Trim();

            // Нормализуем сломанные варианты
            value = RepairBrokenDate(value);

            var formats = new[]
            {
                "dd.MM.yyyy",
                "ddMMyyyy"
            };

            if (DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
            {
                return parsed;
            }

            if (Regex.IsMatch(value, @"^\d{2}\.\d{6}$"))
            {
                value = value.Insert(5, ".");
                if (DateTime.TryParseExact(
                    value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
                {
                    return parsed;
                }
            }

            if (Regex.IsMatch(value, @"^\d{8}$"))
            {
                value = value.Insert(2, ".").Insert(5, ".");
                if (DateTime.TryParseExact(
                    value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
                {
                    return parsed;
                }
            }

            throw new InvalidOperationException($"Не удалось распарсить дату: {rawDate}");
        }

        private static string RepairBrokenDate(string value)
        {
            // Уже нормальная дата
            if (Regex.IsMatch(value, @"^\d{2}\.\d{2}\.\d{4}$"))
            {
                return value;
            }

            // 01.022022 -> 01.02.2022
            if (Regex.IsMatch(value, @"^\d{2}\.\d{6}$"))
            {
                return value.Insert(5, ".");
            }

            // 01022022 -> 01.02.2022
            if (Regex.IsMatch(value, @"^\d{8}$"))
            {
                return value.Insert(2, ".").Insert(5, ".");
            }

            // 01.02.202 -> 01.02.2022
            if (Regex.IsMatch(value, @"^\d{2}\.\d{2}\.\d{3}$"))
            {
                return $"{value}2";
            }

            // 01.02202 -> 01.02.2022
            if (Regex.IsMatch(value, @"^\d{2}\.\d{5}$"))
            {
                var fixedValue = value.Insert(5, ".");
                if (Regex.IsMatch(fixedValue, @"^\d{2}\.\d{2}\.\d{3}$"))
                {
                    return $"{fixedValue}2";
                }
            }

            // 0102202 -> 01.02.2022
            if (Regex.IsMatch(value, @"^\d{7}$"))
            {
                var fixedValue = value.Insert(2, ".").Insert(5, ".");
                if (Regex.IsMatch(fixedValue, @"^\d{2}\.\d{2}\.\d{3}$"))
                {
                    return $"{fixedValue}2";
                }
            }

            return value;
        }

        private static string NormalizeDateText(string value)
        {
            var normalized = NormalizeText(value);

            normalized = Regex.Replace(normalized, @"\bС\b", "с", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\bПО\b", "по", RegexOptions.IgnoreCase);

            normalized = Regex.Replace(normalized, @"с\s+(\d{2})\.(\d{6})", "с $1.$2");
            normalized = Regex.Replace(normalized, @"по\s+(\d{2})\.(\d{6})", "по $1.$2");

            // Иногда точка перед годом теряется полностью
            normalized = Regex.Replace(normalized, @"с\s+(\d{2}\.\d{2}\.\d{3})(?!\d)", "с $12");
            normalized = Regex.Replace(normalized, @"по\s+(\d{2}\.\d{2}\.\d{3})(?!\d)", "по $12");

            return normalized;
        }

        private static string CleanJournalTitle(string rawTitle, List<string> issns)
        {
            var title = NormalizeText(rawTitle);

            foreach (var issn in issns)
            {
                if (!string.IsNullOrWhiteSpace(issn))
                {
                    title = title.Replace(issn, " ", StringComparison.OrdinalIgnoreCase);
                }
            }

            // удаляем длинные служебные пояснения
            title = Regex.Replace(title, @"\(\s*перевод\s+наименования.*?\)", "", RegexOptions.IgnoreCase);
            title = Regex.Replace(title, @"\(\s*перевод\s+названия.*?\)", "", RegexOptions.IgnoreCase);
            title = Regex.Replace(title, @"\(\s*до\s+\d{2}\.\d{2}\.\d{4}.*?\)", "", RegexOptions.IgnoreCase);
            title = Regex.Replace(title, @"\(\s*До\s+\d{2}\.\d{2}\.\d{4}.*?\)", "", RegexOptions.IgnoreCase);

            title = FixBrokenRussianKh(title);

            title = Regex.Replace(title, @"\s+", " ").Trim();
            title = title.Trim(',', ';', '.', ' ');

            return title;
        }

        private static string FixBrokenRussianKh(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var value = text;

            // Латинская X внутри русских слов -> русская х
            value = Regex.Replace(value, @"(?<=[А-Яа-яЁё])X(?=[А-Яа-яЁё])", "х");
            value = Regex.Replace(value, @"(?<=[А-Яа-яЁё])Х(?=[А-Яа-яЁё])", "х");

            // Русская Х внутри латиницы -> латинская X
            value = Regex.Replace(value, @"(?<=[A-Za-z])Х(?=[A-Za-z])", "X");
            value = Regex.Replace(value, @"(?<=[A-Za-z])х(?=[A-Za-z])", "x");

            // Чаще всего после pdf-грязи нужна заглавная русская Х в начале слова
            value = Regex.Replace(value, @"\bарх", "Арх");
            value = Regex.Replace(value, @"\bхим", "Хим");
            value = Regex.Replace(value, @"\bхир", "Хир");

            return value;
        }

        private sealed class RawWord
        {
            public string Text { get; set; } = string.Empty;
            public double X { get; set; }
        }

        private sealed class RawLine
        {
            public double Y { get; set; }
            public List<RawWord> Words { get; set; } = new();
        }

        private sealed class SplitLine
        {
            public string Number { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Issn { get; set; } = string.Empty;
            public string Specialties { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
        }

        private sealed class RawVakJournalRow
        {
            public string Number { get; set; } = string.Empty;
            public List<string> TitleParts { get; set; } = new();
            public List<string> IssnParts { get; set; } = new();
            public List<RawVakSpecialtyRow> SpecialtyRows { get; set; } = new();
        }

        private sealed class RawVakSpecialtyRow
        {
            public string SpecialtyText { get; set; } = string.Empty;
            public string DateText { get; set; } = string.Empty;
        }

        private sealed class SpecialtyBlock
        {
            public string SpecialtyText { get; set; } = string.Empty;
            public string DateText { get; set; } = string.Empty;
        }
    }
}
