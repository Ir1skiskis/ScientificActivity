using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IPublicationModel : IId
    {
        string Title { get; }
        string Authors { get; }

        int Year { get; }
        DateTime? PublicationDate { get; }

        PublicationType Type { get; }

        string? Doi { get; }
        string? Url { get; }

        int? JournalId { get; }
        int? ConferenceId { get; }

        int ResearcherId { get; }

        string? Keywords { get; }
        string? Annotation { get; }
    }
}
