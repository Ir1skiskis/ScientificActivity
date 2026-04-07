using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IGrantModel : IId
    {
        string ContestNumber { get; }
        string Title { get; }
        string? Description { get; }

        string Organization { get; }

        DateTime StartDate { get; }
        DateTime EndDate { get; }

        decimal? Amount { get; }
        string? Currency { get; }

        string? SubjectArea { get; }
        GrantStatus Status { get; }

        string? Url { get; }
    }
}
