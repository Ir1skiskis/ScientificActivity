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

        public string? ELibraryId { get; set; }

        public int? CitationsRincCount { get; set; }

        public bool IsInRinc { get; set; }
        public bool IsInCoreRinc { get; set; }

        public bool IsWhiteListLevel1 { get; set; }
        public bool IsWhiteListLevel2 { get; set; }
        public bool IsWhiteListLevel3 { get; set; }
        public bool IsWhiteListLevel4 { get; set; }

        public bool IsRsci { get; set; }

        public bool IsScopusQ1 { get; set; }
        public bool IsScopusQ2 { get; set; }
        public bool IsScopusQ3 { get; set; }
        public bool IsScopusQ4 { get; set; }

        public bool IsWebOfScienceQ1 { get; set; }
        public bool IsWebOfScienceQ2 { get; set; }
        public bool IsWebOfScienceQ3 { get; set; }
        public bool IsWebOfScienceQ4 { get; set; }
        public bool IsWebOfScienceNoQuartile { get; set; }

        public bool IsVak { get; set; }
        public bool IsVakCategory1 { get; set; }
        public bool IsVakCategory2 { get; set; }
        public bool IsVakCategory3 { get; set; }

        public string? RubricOecd { get; set; }
        public string? RubricAsjc { get; set; }
        public string? RubricGrnti { get; set; }
        public string? VakSpecialty { get; set; }

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
                Annotation = model.Annotation,
                ELibraryId = model.ELibraryId,
                CitationsRincCount = model.CitationsRincCount,
                IsInRinc = model.IsInRinc,
                IsInCoreRinc = model.IsInCoreRinc,
                IsWhiteListLevel1 = model.IsWhiteListLevel1,
                IsWhiteListLevel2 = model.IsWhiteListLevel2,
                IsWhiteListLevel3 = model.IsWhiteListLevel3,
                IsWhiteListLevel4 = model.IsWhiteListLevel4,
                IsRsci = model.IsRsci,
                IsScopusQ1 = model.IsScopusQ1,
                IsScopusQ2 = model.IsScopusQ2,
                IsScopusQ3 = model.IsScopusQ3,
                IsScopusQ4 = model.IsScopusQ4,
                IsWebOfScienceQ1 = model.IsWebOfScienceQ1,
                IsWebOfScienceQ2 = model.IsWebOfScienceQ2,
                IsWebOfScienceQ3 = model.IsWebOfScienceQ3,
                IsWebOfScienceQ4 = model.IsWebOfScienceQ4,
                IsWebOfScienceNoQuartile = model.IsWebOfScienceNoQuartile,
                IsVak = model.IsVak,
                IsVakCategory1 = model.IsVakCategory1,
                IsVakCategory2 = model.IsVakCategory2,
                IsVakCategory3 = model.IsVakCategory3,
                RubricOecd = model.RubricOecd,
                RubricAsjc = model.RubricAsjc,
                RubricGrnti = model.RubricGrnti,
                VakSpecialty = model.VakSpecialty
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
            ELibraryId = model.ELibraryId;
            CitationsRincCount = model.CitationsRincCount;
            IsInRinc = model.IsInRinc;
            IsInCoreRinc = model.IsInCoreRinc;
            IsWhiteListLevel1 = model.IsWhiteListLevel1;
            IsWhiteListLevel2 = model.IsWhiteListLevel2;
            IsWhiteListLevel3 = model.IsWhiteListLevel3;
            IsWhiteListLevel4 = model.IsWhiteListLevel4;
            IsRsci = model.IsRsci;
            IsScopusQ1 = model.IsScopusQ1;
            IsScopusQ2 = model.IsScopusQ2;
            IsScopusQ3 = model.IsScopusQ3;
            IsScopusQ4 = model.IsScopusQ4;
            IsWebOfScienceQ1 = model.IsWebOfScienceQ1;
            IsWebOfScienceQ2 = model.IsWebOfScienceQ2;
            IsWebOfScienceQ3 = model.IsWebOfScienceQ3;
            IsWebOfScienceQ4 = model.IsWebOfScienceQ4;
            IsWebOfScienceNoQuartile = model.IsWebOfScienceNoQuartile;
            IsVak = model.IsVak;
            IsVakCategory1 = model.IsVakCategory1;
            IsVakCategory2 = model.IsVakCategory2;
            IsVakCategory3 = model.IsVakCategory3;
            RubricOecd = model.RubricOecd;
            RubricAsjc = model.RubricAsjc;
            RubricGrnti = model.RubricGrnti;
            VakSpecialty = model.VakSpecialty;
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
            ELibraryId = ELibraryId,
            CitationsRincCount = CitationsRincCount,
            IsInRinc = IsInRinc,
            IsInCoreRinc = IsInCoreRinc,
            IsRsci = IsRsci,
            IsScopusQ1 = IsScopusQ1,
            IsScopusQ2 = IsScopusQ2,
            IsScopusQ3 = IsScopusQ3,
            IsScopusQ4 = IsScopusQ4,
            IsVak = IsVak,
            IsVakCategory1 = IsVakCategory1,
            IsVakCategory2 = IsVakCategory2,
            IsVakCategory3 = IsVakCategory3,
            IsWebOfScienceNoQuartile = IsWebOfScienceNoQuartile,
            IsWebOfScienceQ1 = IsWebOfScienceQ1,
            IsWebOfScienceQ2 = IsWebOfScienceQ2,
            IsWebOfScienceQ3 = IsWebOfScienceQ3,
            IsWebOfScienceQ4 = IsWebOfScienceQ4,
            IsWhiteListLevel1 = IsWhiteListLevel1,
            IsWhiteListLevel2 = IsWhiteListLevel2,
            IsWhiteListLevel3 = IsWhiteListLevel3,
            IsWhiteListLevel4 = IsWhiteListLevel4,
            VakSpecialty = VakSpecialty,
            RubricAsjc = RubricAsjc,
            RubricGrnti = RubricGrnti,
            RubricOecd = RubricOecd,
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
