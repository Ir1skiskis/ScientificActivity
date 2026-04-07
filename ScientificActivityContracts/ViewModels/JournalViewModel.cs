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
    public class JournalViewModel : IJournalModel
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        public string Title { get; set; } = string.Empty;

        [DisplayName("ISSN")]
        public string? Issn { get; set; }

        [DisplayName("E-ISSN")]
        public string? EIssn { get; set; }

        [DisplayName("Издатель")]
        public string? Publisher { get; set; }

        [DisplayName("Тематика")]
        public string? SubjectArea { get; set; }

        [DisplayName("Квартиль")]
        public JournalQuartile Quartile { get; set; }

        [DisplayName("Входит в ВАК")]
        public bool IsVak { get; set; }

        [DisplayName("Входит в Белый список")]
        public bool IsWhiteList { get; set; }

        [DisplayName("Страна")]
        public string? Country { get; set; }

        [DisplayName("Ссылка")]
        public string? Url { get; set; }
    }
}
