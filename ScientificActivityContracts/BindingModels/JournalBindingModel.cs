using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class JournalBindingModel : IJournalModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Issn { get; set; }
        public string? EIssn { get; set; }
        public string? Publisher { get; set; }
        public string? SubjectArea { get; set; }

        public bool IsVak { get; set; }
        public bool IsWhiteList { get; set; }

        public int? WhiteListLevel2023 { get; set; }
        public int? WhiteListLevel2025 { get; set; }
        public string? WhiteListState { get; set; }
        public string? WhiteListNotice { get; set; }
        public DateTime? WhiteListAcceptedDate { get; set; }
        public DateTime? WhiteListDiscontinuedDate { get; set; }

        public string? Country { get; set; }
        public string? Url { get; set; }

        public int? RcsiRecordSourceId { get; set; }
    }
}
