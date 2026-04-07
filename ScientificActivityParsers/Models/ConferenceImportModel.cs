using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class ConferenceImportModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Organizer { get; set; }
        public string? SubjectArea { get; set; }
        public string? Url { get; set; }
    }
}
