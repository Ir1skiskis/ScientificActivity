using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IConferenceModel : IId
    {
        string Title { get; }
        string? Description { get; }

        DateTime StartDate { get; }
        DateTime EndDate { get; }

        string? City { get; }
        string? Country { get; }
        string? Organizer { get; }

        string? SubjectArea { get; }
        ConferenceFormat Format { get; }
        ConferenceLevel Level { get; }

        string? Url { get; }
    }
}
