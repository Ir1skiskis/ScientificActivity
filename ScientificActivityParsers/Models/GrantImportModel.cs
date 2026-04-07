using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class GrantImportModel
    {
        public string ContestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Organization { get; set; } = string.Empty;
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime? ResultDate { get; set; }
        public string? SubjectArea { get; set; }
        public string? Url { get; set; }
        public string? StatusText { get; set; }
    }
}
