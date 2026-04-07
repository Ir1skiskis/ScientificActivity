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
    public class ConferenceViewModel : IConferenceModel
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        public string Title { get; set; } = string.Empty;

        [DisplayName("Описание")]
        public string? Description { get; set; }

        [DisplayName("Дата начала")]
        public DateTime StartDate { get; set; }

        [DisplayName("Дата окончания")]
        public DateTime EndDate { get; set; }

        [DisplayName("Город")]
        public string? City { get; set; }

        [DisplayName("Страна")]
        public string? Country { get; set; }

        [DisplayName("Организатор")]
        public string? Organizer { get; set; }

        [DisplayName("Тематика")]
        public string? SubjectArea { get; set; }

        [DisplayName("Формат")]
        public ConferenceFormat Format { get; set; }

        [DisplayName("Уровень")]
        public ConferenceLevel Level { get; set; }

        [DisplayName("Ссылка")]
        public string? Url { get; set; }

        [DisplayName("Период проведения")]
        public string DateRange => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
    }
}
