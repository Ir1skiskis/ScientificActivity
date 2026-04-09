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

        [DisplayName("EISSN")]
        public string? EIssn { get; set; }

        [DisplayName("Издатель")]
        public string? Publisher { get; set; }

        [DisplayName("Тематика")]
        public string? SubjectArea { get; set; }

        [DisplayName("ВАК")]
        public bool IsVak { get; set; }

        [DisplayName("Белый список")]
        public bool IsWhiteList { get; set; }

        [DisplayName("Уровень БС 2023")]
        public int? WhiteListLevel2023 { get; set; }

        [DisplayName("Уровень БС 2025")]
        public int? WhiteListLevel2025 { get; set; }

        [DisplayName("Статус БС")]
        public string? WhiteListState { get; set; }

        [DisplayName("Примечание БС")]
        public string? WhiteListNotice { get; set; }

        [DisplayName("Дата включения в БС")]
        public DateTime? WhiteListAcceptedDate { get; set; }

        [DisplayName("Дата исключения из БС")]
        public DateTime? WhiteListDiscontinuedDate { get; set; }

        [DisplayName("Страна")]
        public string? Country { get; set; }

        [DisplayName("Ссылка")]
        public string? Url { get; set; }

        [DisplayName("ID РЦНИ")]
        public int? RcsiRecordSourceId { get; set; }
    }
}
