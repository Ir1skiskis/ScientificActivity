using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class GrantSearchModel
    {
        public int? Id { get; set; }

        public string? ContestNumber { get; set; }

        public string? Title { get; set; }

        public string? Organization { get; set; }

        public string? SubjectArea { get; set; }

        public GrantStatus? Status { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
