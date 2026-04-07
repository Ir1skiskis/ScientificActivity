using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class Researcher : IResearcherModel
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Исследователь;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        [Required]
        public AcademicDegree AcademicDegree { get; set; } = AcademicDegree.Не_указано;

        public string? ELibraryAuthorId { get; set; }

        public string? ResearchTopics { get; set; }

        public virtual List<Publication> Publications { get; set; } = new();

        public virtual List<ResearcherInterest> Interests { get; set; } = new();

        public static Researcher? Create(ResearcherBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Researcher
            {
                Id = model.Id,
                Email = model.Email,
                PasswordHash = model.PasswordHash,
                Role = model.Role,
                IsActive = model.IsActive,
                LastName = model.LastName,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                Phone = model.Phone,
                Department = model.Department,
                Position = model.Position,
                AcademicDegree = model.AcademicDegree,
                ELibraryAuthorId = model.ELibraryAuthorId,
                ResearchTopics = model.ResearchTopics
            };
        }

        public void Update(ResearcherBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            Email = model.Email;
            Role = model.Role;
            IsActive = model.IsActive;
            LastName = model.LastName;
            FirstName = model.FirstName;
            MiddleName = model.MiddleName;
            Phone = model.Phone;
            Department = model.Department;
            Position = model.Position;
            AcademicDegree = model.AcademicDegree;
            ELibraryAuthorId = model.ELibraryAuthorId;
            ResearchTopics = model.ResearchTopics;

            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                PasswordHash = model.PasswordHash;
            }
        }

        public ResearcherViewModel GetViewModel => new()
        {
            Id = Id,
            Email = Email,
            PasswordHash = string.Empty,
            Role = Role,
            IsActive = IsActive,
            LastName = LastName,
            FirstName = FirstName,
            MiddleName = MiddleName,
            Phone = Phone,
            Department = Department,
            Position = Position,
            AcademicDegree = AcademicDegree,
            ELibraryAuthorId = ELibraryAuthorId,
            ResearchTopics = ResearchTopics,
            PublicationsCount = Publications?.Count ?? 0
        };
    }
}
