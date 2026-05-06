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

        string? ELibraryId { get; }

        int? CitationsRincCount { get; }

        bool IsInRinc { get; }
        bool IsInCoreRinc { get; }

        bool IsWhiteListLevel1 { get; }
        bool IsWhiteListLevel2 { get; }
        bool IsWhiteListLevel3 { get; }
        bool IsWhiteListLevel4 { get; }

        bool IsRsci { get; }

        bool IsScopusQ1 { get; }
        bool IsScopusQ2 { get; }
        bool IsScopusQ3 { get; }
        bool IsScopusQ4 { get; }

        bool IsWebOfScienceQ1 { get; }
        bool IsWebOfScienceQ2 { get; }
        bool IsWebOfScienceQ3 { get; }
        bool IsWebOfScienceQ4 { get; }
        bool IsWebOfScienceNoQuartile { get; }

        bool IsVak { get; }
        bool IsVakCategory1 { get; }
        bool IsVakCategory2 { get; }
        bool IsVakCategory3 { get; }

        string? RubricOecd { get; }
        string? RubricAsjc { get; }
        string? RubricGrnti { get; }
        string? VakSpecialty { get; }
    }
}
