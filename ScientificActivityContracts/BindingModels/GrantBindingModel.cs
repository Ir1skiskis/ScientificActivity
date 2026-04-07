using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class GrantBindingModel : IGrantModel
    {
        public int Id { get; set; }

        public string ContestNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Organization { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal? Amount { get; set; }

        public string? Currency { get; set; }

        public string? SubjectArea { get; set; }

        public GrantStatus Status { get; set; } = GrantStatus.Открыт;

        public string? Url { get; set; }
    }
}
