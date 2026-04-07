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
    public class Conference : IConferenceModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Organizer { get; set; }

        public string? SubjectArea { get; set; }

        [Required]
        public ConferenceFormat Format { get; set; } = ConferenceFormat.Не_указан;

        [Required]
        public ConferenceLevel Level { get; set; } = ConferenceLevel.Не_указан;

        public string? Url { get; set; }

        public virtual List<Publication> Publications { get; set; } = new();

        public static Conference? Create(ConferenceBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Conference
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                City = model.City,
                Country = model.Country,
                Organizer = model.Organizer,
                SubjectArea = model.SubjectArea,
                Format = model.Format,
                Level = model.Level,
                Url = model.Url
            };
        }

        public void Update(ConferenceBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            Title = model.Title;
            Description = model.Description;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            City = model.City;
            Country = model.Country;
            Organizer = model.Organizer;
            SubjectArea = model.SubjectArea;
            Format = model.Format;
            Level = model.Level;
            Url = model.Url;
        }

        public ConferenceViewModel GetViewModel => new()
        {
            Id = Id,
            Title = Title,
            Description = Description,
            StartDate = StartDate,
            EndDate = EndDate,
            City = City,
            Country = Country,
            Organizer = Organizer,
            SubjectArea = SubjectArea,
            Format = Format,
            Level = Level,
            Url = Url
        };
    }
}
