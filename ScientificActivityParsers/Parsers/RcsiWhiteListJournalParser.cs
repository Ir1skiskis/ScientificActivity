using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Parsers
{
    public class RcsiWhiteListJournalParser : IWhiteListJournalParser
    {
        private readonly IRcsiApiClient _rcsiApiClient;

        public RcsiWhiteListJournalParser(IRcsiApiClient rcsiApiClient)
        {
            _rcsiApiClient = rcsiApiClient;
        }

        public async Task<List<JournalImportModel>> ParseAsync(CancellationToken cancellationToken = default)
        {
            var items = await _rcsiApiClient.GetAllRecordSourcesAsync(cancellationToken);
            var result = new List<JournalImportModel>();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var normalizedIssns = item.Issns
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(SplitIssns)
                    .Select(NormalizeIssn)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var title = item.Title
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine("RCSI SKIP: empty title");
                    continue;
                }

                result.Add(new JournalImportModel
                {
                    Title = title,
                    Issn = normalizedIssns.ElementAtOrDefault(0),
                    EIssn = normalizedIssns.ElementAtOrDefault(1),
                    IsVak = false,
                    SourceName = "Белый список РЦНИ",
                    SourceActualDate = DateTime.UtcNow.Date,
                    SubjectArea = null,
                    WhiteListLevel2023 = item.Level2023,
                    WhiteListLevel2025 = item.Level2025,
                    WhiteListState = item.State,
                    WhiteListNotice = item.Notice,
                    WhiteListAcceptedDate = ParseNullableDate(item.DateAccepted),
                    WhiteListDiscontinuedDate = ParseNullableDate(item.DateDiscontinued),
                    Url = null,
                    RcsiRecordSourceId = null,
                    AllIssns = normalizedIssns
                });
            }

            return result;
        }

        private static IEnumerable<string> SplitIssns(string value)
        {
            return value
                .Split(new[] { '|', ';', ',', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private static string NormalizeIssn(string value)
        {
            return value.Trim().ToUpperInvariant().Replace('Х', 'X').Replace('х', 'X');
        }

        private static DateTime? ParseNullableDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var formats = new[]
            {
                "yyyy-MM-dd",
                "dd.MM.yyyy"
            };

            if (DateTime.TryParseExact(
                value.Trim(),
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
