using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IJournalModel : IId
    {
        string Title { get; }
        string? Issn { get; }
        string? EIssn { get; }
        string? Publisher { get; }

        string? SubjectArea { get; }

        int? WhiteListLevel2023 { get; }
        int? WhiteListLevel2025 { get; }
        string? WhiteListState { get; }
        string? WhiteListNotice { get; }
        DateTime? WhiteListAcceptedDate { get; }
        DateTime? WhiteListDiscontinuedDate { get; }

        bool IsVak { get; }
        bool IsWhiteList { get; }

        string? Country { get; }
        string? Url { get; }

        int? RcsiRecordSourceId { get; }
    }
}
