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
    public class Publication : IPublicationModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Authors { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        public DateTime? PublicationDate { get; set; }

        [Required]
        public PublicationType Type { get; set; } = PublicationType.Статья_в_журнале;

        public string? Doi { get; set; }

        public string? Url { get; set; }

        public int? JournalId { get; set; }

        public int? ConferenceId { get; set; }

        [Required]
        public int ResearcherId { get; set; }

        public string? Keywords { get; set; }

        public string? Annotation { get; set; }

        public virtual Researcher Researcher { get; set; } = null!;

        public virtual Journal? Journal { get; set; }

        public virtual Conference? Conference { get; set; }

        public static Publication? Create(PublicationBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Publication
            {
                Id = model.Id,
                Title = model.Title,
                Authors = model.Authors,
                Year = model.Year,
                PublicationDate = model.PublicationDate,
                Type = model.Type,
                Doi = model.Doi,
                Url = model.Url,
                JournalId = model.JournalId,
                ConferenceId = model.ConferenceId,
                ResearcherId = model.ResearcherId,
                Keywords = model.Keywords,
                Annotation = model.Annotation
            };
        }

        public void Update(PublicationBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            Title = model.Title;
            Authors = model.Authors;
            Year = model.Year;
            PublicationDate = model.PublicationDate;
            Type = model.Type;
            Doi = model.Doi;
            Url = model.Url;
            JournalId = model.JournalId;
            ConferenceId = model.ConferenceId;
            ResearcherId = model.ResearcherId;
            Keywords = model.Keywords;
            Annotation = model.Annotation;
        }

        public PublicationViewModel GetViewModel => new()
        {
            Id = Id,
            Title = Title,
            Authors = Authors,
            Year = Year,
            PublicationDate = PublicationDate,
            Type = Type,
            Doi = Doi,
            Url = Url,
            JournalId = JournalId,
            ConferenceId = ConferenceId,
            ResearcherId = ResearcherId,
            Keywords = Keywords,
            Annotation = Annotation,
            ResearcherFullName = Researcher == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(Researcher.MiddleName)
                    ? $"{Researcher.LastName} {Researcher.FirstName}"
                    : $"{Researcher.LastName} {Researcher.FirstName} {Researcher.MiddleName}",
            JournalTitle = Journal?.Title,
            ConferenceTitle = Conference?.Title
        };
    }
}
