using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ConferenceBindingModel : IConferenceModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Organizer { get; set; }

        public string? SubjectArea { get; set; }

        public ConferenceFormat Format { get; set; } = ConferenceFormat.Не_указан;

        public ConferenceLevel Level { get; set; } = ConferenceLevel.Не_указан;

        public string? Url { get; set; }
    }
}
