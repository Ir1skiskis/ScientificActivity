using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class PublicationViewModel : IPublicationModel
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        public string Title { get; set; } = string.Empty;

        [DisplayName("Авторы")]
        public string Authors { get; set; } = string.Empty;

        [DisplayName("Год")]
        public int Year { get; set; }

        [DisplayName("Дата публикации")]
        public DateTime? PublicationDate { get; set; }

        [DisplayName("Тип публикации")]
        public PublicationType Type { get; set; }

        [DisplayName("DOI")]
        public string? Doi { get; set; }

        [DisplayName("Ссылка")]
        public string? Url { get; set; }

        public int? JournalId { get; set; }

        public int? ConferenceId { get; set; }

        public int ResearcherId { get; set; }

        [DisplayName("Ключевые слова")]
        public string? Keywords { get; set; }

        [DisplayName("Аннотация")]
        public string? Annotation { get; set; }

        [DisplayName("Исследователь")]
        public string ResearcherFullName { get; set; } = string.Empty;

        [DisplayName("Журнал")]
        public string? JournalTitle { get; set; }

        [DisplayName("Конференция")]
        public string? ConferenceTitle { get; set; }

        public string? ELibraryId { get; set; }

        public int? CitationsRincCount { get; set; }

        public bool IsInRinc { get; set; }
        public bool IsInCoreRinc { get; set; }

        public bool IsWhiteListLevel1 { get; set; }
        public bool IsWhiteListLevel2 { get; set; }
        public bool IsWhiteListLevel3 { get; set; }
        public bool IsWhiteListLevel4 { get; set; }

        public bool IsRsci { get; set; }

        public bool IsScopusQ1 { get; set; }
        public bool IsScopusQ2 { get; set; }
        public bool IsScopusQ3 { get; set; }
        public bool IsScopusQ4 { get; set; }

        public bool IsWebOfScienceQ1 { get; set; }
        public bool IsWebOfScienceQ2 { get; set; }
        public bool IsWebOfScienceQ3 { get; set; }
        public bool IsWebOfScienceQ4 { get; set; }
        public bool IsWebOfScienceNoQuartile { get; set; }

        public bool IsVak { get; set; }
        public bool IsVakCategory1 { get; set; }
        public bool IsVakCategory2 { get; set; }
        public bool IsVakCategory3 { get; set; }

        public string? RubricOecd { get; set; }
        public string? RubricAsjc { get; set; }
        public string? RubricGrnti { get; set; }
        public string? VakSpecialty { get; set; }
    }
}
