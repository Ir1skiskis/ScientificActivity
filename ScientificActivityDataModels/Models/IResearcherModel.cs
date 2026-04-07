using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IResearcherModel : IUserModel
    {
        string LastName { get; }
        string FirstName { get; }
        string? MiddleName { get; }
        string Phone { get; }

        string Department { get; }
        string Position { get; }
        AcademicDegree AcademicDegree { get; }

        string? ELibraryAuthorId { get; }

        string? ResearchTopics { get; }
    }
}
