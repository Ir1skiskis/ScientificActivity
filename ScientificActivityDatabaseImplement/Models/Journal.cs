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

        public int? WhiteListLevel2023 { get; set; }
        public int? WhiteListLevel2025 { get; set; }
        public string? WhiteListState { get; set; }
        public string? WhiteListNotice { get; set; }
        public DateTime? WhiteListAcceptedDate { get; set; }
        public DateTime? WhiteListDiscontinuedDate { get; set; }

        [Required]
        public bool IsVak { get; set; }

        [Required]
        public bool IsWhiteList { get; set; }

        public string? Country { get; set; }

        public string? Url { get; set; }

        public int? RcsiRecordSourceId { get; set; }

        public virtual List<Publication> Publications { get; set; } = new();

        public virtual List<JournalVakSpecialty> VakSpecialties { get; set; } = new();

        public static Journal? Create(JournalBindingModel? model)
        {
            if (model == null)
            {
                return null;
            }

            return new Journal
            {
                Id = model.Id,
                RcsiRecordSourceId = model.RcsiRecordSourceId,
                Title = model.Title,
                Issn = model.Issn,
                EIssn = model.EIssn,
                Publisher = model.Publisher,
                SubjectArea = model.SubjectArea,
                IsVak = model.IsVak,
                IsWhiteList = model.IsWhiteList,
                WhiteListLevel2023 = model.WhiteListLevel2023,
                WhiteListLevel2025 = model.WhiteListLevel2025,
                WhiteListState = model.WhiteListState,
                WhiteListNotice = model.WhiteListNotice,
                WhiteListAcceptedDate = model.WhiteListAcceptedDate,
                WhiteListDiscontinuedDate = model.WhiteListDiscontinuedDate,
                Country = model.Country,
                Url = model.Url
            };
        }

        public void Update(JournalBindingModel model)
        {
            Title = model.Title;
            RcsiRecordSourceId = model.RcsiRecordSourceId;
            Issn = model.Issn;
            EIssn = model.EIssn;
            Publisher = model.Publisher;
            SubjectArea = model.SubjectArea;
            IsVak = model.IsVak;
            IsWhiteList = model.IsWhiteList;
            WhiteListLevel2023 = model.WhiteListLevel2023;
            WhiteListLevel2025 = model.WhiteListLevel2025;
            WhiteListState = model.WhiteListState;
            WhiteListNotice = model.WhiteListNotice;
            WhiteListAcceptedDate = model.WhiteListAcceptedDate;
            WhiteListDiscontinuedDate = model.WhiteListDiscontinuedDate;
            Country = model.Country;
            Url = model.Url;
        }

        public JournalViewModel GetViewModel => new()
        {
            Id = Id,
            RcsiRecordSourceId = RcsiRecordSourceId,
            Title = Title,
            Issn = Issn,
            EIssn = EIssn,
            Publisher = Publisher,
            SubjectArea = SubjectArea,
            IsVak = IsVak,
            IsWhiteList = IsWhiteList,
            WhiteListLevel2023 = WhiteListLevel2023,
            WhiteListLevel2025 = WhiteListLevel2025,
            WhiteListState = WhiteListState,
            WhiteListNotice = WhiteListNotice,
            WhiteListAcceptedDate = WhiteListAcceptedDate,
            WhiteListDiscontinuedDate = WhiteListDiscontinuedDate,
            Country = Country,
            Url = Url
        };
    }
}
