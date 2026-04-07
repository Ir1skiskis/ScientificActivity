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
    public class ResearcherViewModel : IResearcherModel
    {
        public int Id { get; set; }

        [DisplayName("Email")]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [DisplayName("Роль")]
        public UserRole Role { get; set; }

        [DisplayName("Активен")]
        public bool IsActive { get; set; }

        [DisplayName("Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [DisplayName("Имя")]
        public string FirstName { get; set; } = string.Empty;

        [DisplayName("Отчество")]
        public string? MiddleName { get; set; }

        [DisplayName("Телефон")]
        public string Phone { get; set; } = string.Empty;

        [DisplayName("Кафедра / подразделение")]
        public string Department { get; set; } = string.Empty;

        [DisplayName("Должность")]
        public string Position { get; set; } = string.Empty;

        [DisplayName("Учёная степень")]
        public AcademicDegree AcademicDegree { get; set; }

        [DisplayName("ID автора eLibrary")]
        public string? ELibraryAuthorId { get; set; }

        [DisplayName("Научные интересы")]
        public string? ResearchTopics { get; set; }

        [DisplayName("ФИО")]
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{LastName} {FirstName}"
            : $"{LastName} {FirstName} {MiddleName}";

        [DisplayName("Количество публикаций")]
        public int PublicationsCount { get; set; }
    }
}
