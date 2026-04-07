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
    public class GrantViewModel : IGrantModel
    {
        public int Id { get; set; }

        [DisplayName("Номер конкурса")]
        public string ContestNumber { get; set; } = string.Empty;

        [DisplayName("Название")]
        public string Title { get; set; } = string.Empty;

        [DisplayName("Описание")]
        public string? Description { get; set; }

        [DisplayName("Организация")]
        public string Organization { get; set; } = string.Empty;

        [DisplayName("Дата начала")]
        public DateTime StartDate { get; set; }

        [DisplayName("Дата окончания")]
        public DateTime EndDate { get; set; }

        [DisplayName("Сумма")]
        public decimal? Amount { get; set; }

        [DisplayName("Валюта")]
        public string? Currency { get; set; }

        [DisplayName("Тематика")]
        public string? SubjectArea { get; set; }

        [DisplayName("Статус")]
        public GrantStatus Status { get; set; }

        [DisplayName("Ссылка")]
        public string? Url { get; set; }

        [DisplayName("Период конкурса")]
        public string DateRange => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
    }
}
