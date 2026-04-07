using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ResearcherBindingModel : IResearcherModel
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Исследователь;

        public bool IsActive { get; set; } = true;

        public string LastName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public AcademicDegree AcademicDegree { get; set; } = AcademicDegree.Не_указано;

        public string? ELibraryAuthorId { get; set; }

        public string? ResearchTopics { get; set; }
    }
}
