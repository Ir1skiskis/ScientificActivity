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
        JournalQuartile Quartile { get; }

        bool IsVak { get; }
        bool IsWhiteList { get; }

        string? Country { get; }
        string? Url { get; }
    }
}
