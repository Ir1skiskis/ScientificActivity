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
    public class Journal : IJournalModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Issn { get; set; }

        public string? EIssn { get; set; }

        public string? Publisher { get; set; }

        public string? SubjectArea { get; set; }

        [Required]
        public JournalQuartile Quartile { get; set; } = JournalQuartile.Не_указан;

        [Required]
        public bool IsVak { get; set; }

        [Required]
        public bool IsWhiteList { get; set; }

        public string? Country { get; set; }

        public string? Url { get; set; }

        public virtual List<Publication> Publications { get; set; } = new();

        public virtual List<JournalVakSpecialty> VakSpecialties { get; set; } = new();

        public static Journal? Create(JournalBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Journal
            {
                Id = model.Id,
                Title = model.Title,
                Issn = model.Issn,
                EIssn = model.EIssn,
                Publisher = model.Publisher,
                SubjectArea = model.SubjectArea,
                Quartile = model.Quartile,
                IsVak = model.IsVak,
                IsWhiteList = model.IsWhiteList,
                Country = model.Country,
                Url = model.Url
            };
        }

        public void Update(JournalBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            Title = model.Title;
            Issn = model.Issn;
            EIssn = model.EIssn;
            Publisher = model.Publisher;
            SubjectArea = model.SubjectArea;
            Quartile = model.Quartile;
            IsVak = model.IsVak;
            IsWhiteList = model.IsWhiteList;
            Country = model.Country;
            Url = model.Url;
        }

        public JournalViewModel GetViewModel => new()
        {
            Id = Id,
            Title = Title,
            Issn = Issn,
            EIssn = EIssn,
            Publisher = Publisher,
            SubjectArea = SubjectArea,
            Quartile = Quartile,
            IsVak = IsVak,
            IsWhiteList = IsWhiteList,
            Country = Country,
            Url = Url
        };
    }
}
