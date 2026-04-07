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
    }
}
