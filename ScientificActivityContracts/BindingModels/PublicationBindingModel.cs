using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class PublicationBindingModel : IPublicationModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Authors { get; set; } = string.Empty;

        public int Year { get; set; }

        public DateTime? PublicationDate { get; set; }

        public PublicationType Type { get; set; } = PublicationType.Статья_в_журнале;

        public string? Doi { get; set; }

        public string? Url { get; set; }

        public int? JournalId { get; set; }

        public int? ConferenceId { get; set; }

        public int ResearcherId { get; set; }

        public string? Keywords { get; set; }

        public string? Annotation { get; set; }
    }
}
