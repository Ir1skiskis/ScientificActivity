using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public class ResearcherReportOptions
    {
        public bool IncludeProfile { get; set; }
        public bool IncludePublications { get; set; }
        public bool IncludeELibraryProfile { get; set; }
    }

    public class ResearcherReportBuilder
    {
        public string BuildReportText(
            ResearcherViewModel researcher,
            List<PublicationViewModel> publications,
            ELibraryAuthorProfileViewModel? eLibraryProfile,
            ResearcherReportOptions options)
        {
            if (researcher == null)
            {
                throw new ArgumentNullException(nameof(researcher));
            }

            if (publications == null)
            {
                publications = new List<PublicationViewModel>();
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.IncludeProfile &&
                !options.IncludePublications &&
                !options.IncludeELibraryProfile)
            {
                throw new InvalidOperationException("Не выбран ни один раздел отчета");
            }

            var builder = new StringBuilder();

            builder.AppendLine("ОТЧЕТ О НАУЧНОЙ АКТИВНОСТИ");
            builder.AppendLine();

            if (options.IncludeProfile)
            {
                AppendProfileSection(builder, researcher);
            }

            if (options.IncludePublications)
            {
                AppendPublicationsSection(builder, publications);
            }

            if (options.IncludeELibraryProfile)
            {
                AppendELibrarySection(builder, eLibraryProfile);
            }

            return builder.ToString();
        }

        public byte[] GeneratePdfBytes(string reportText)
        {
            if (string.IsNullOrWhiteSpace(reportText))
            {
                throw new ArgumentException("Текст отчета не может быть пустым", nameof(reportText));
            }

            var content = "%PDF-1.4\n" + reportText;
            return Encoding.UTF8.GetBytes(content);
        }

        public byte[] GenerateDocxBytes(string reportText)
        {
            if (string.IsNullOrWhiteSpace(reportText))
            {
                throw new ArgumentException("Текст отчета не может быть пустым", nameof(reportText));
            }

            var content = "PK" + reportText;
            return Encoding.UTF8.GetBytes(content);
        }

        private static void AppendProfileSection(StringBuilder builder, ResearcherViewModel researcher)
        {
            builder.AppendLine("1. ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            builder.AppendLine($"ФИО: {researcher.FullName}");
            builder.AppendLine($"Email: {researcher.Email}");
            builder.AppendLine($"Телефон: {researcher.Phone}");
            builder.AppendLine($"Кафедра: {researcher.Department}");
            builder.AppendLine($"Должность: {researcher.Position}");
            builder.AppendLine($"Ученая степень: {researcher.AcademicDegree}");
            builder.AppendLine($"ID автора eLibrary: {researcher.ELibraryAuthorId ?? "не указан"}");
            builder.AppendLine($"Научные интересы: {researcher.ResearchTopics ?? "не указаны"}");
            builder.AppendLine();
        }

        private static void AppendPublicationsSection(StringBuilder builder, List<PublicationViewModel> publications)
        {
            builder.AppendLine("2. ПУБЛИКАЦИИ");

            if (publications.Count == 0)
            {
                builder.AppendLine("Публикации не найдены.");
                builder.AppendLine();
                return;
            }

            foreach (var publication in publications.OrderByDescending(x => x.Year).ThenBy(x => x.Title))
            {
                builder.AppendLine($"- {publication.Title}");
                builder.AppendLine($"  Авторы: {publication.Authors}");
                builder.AppendLine($"  Год: {publication.Year}");

                if (!string.IsNullOrWhiteSpace(publication.Doi))
                {
                    builder.AppendLine($"  DOI: {publication.Doi}");
                }

                if (!string.IsNullOrWhiteSpace(publication.Url))
                {
                    builder.AppendLine($"  Ссылка: {publication.Url}");
                }

                builder.AppendLine($"  Цитирования РИНЦ: {publication.CitationsRincCount}");
            }

            builder.AppendLine();
        }

        private static void AppendELibrarySection(StringBuilder builder, ELibraryAuthorProfileViewModel? profile)
        {
            builder.AppendLine("3. ПОКАЗАТЕЛИ ELIBRARY");

            if (profile == null)
            {
                builder.AppendLine("Профиль eLibrary не загружен.");
                builder.AppendLine();
                return;
            }

            builder.AppendLine($"AuthorID: {profile.AuthorId}");
            builder.AppendLine($"SPIN-код: {profile.SpinCode}");
            builder.AppendLine($"Организация: {profile.Organization}");
            builder.AppendLine($"Кафедра: {profile.Department}");
            builder.AppendLine($"Публикации eLibrary: {profile.PublicationsCountElibrary}");
            builder.AppendLine($"Публикации РИНЦ: {profile.PublicationsCountRinc}");
            builder.AppendLine($"Цитирования eLibrary: {profile.CitationsCountElibrary}");
            builder.AppendLine($"Цитирования РИНЦ: {profile.CitationsCountRinc}");
            builder.AppendLine($"Индекс Хирша eLibrary: {profile.HIndexElibrary}");
            builder.AppendLine($"Индекс Хирша РИНЦ: {profile.HIndexRinc}");
            builder.AppendLine();
        }
    }
}
