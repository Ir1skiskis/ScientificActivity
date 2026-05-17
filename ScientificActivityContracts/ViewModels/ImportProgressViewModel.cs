using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ImportProgressViewModel
    {
        public string JobId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public int Percent { get; set; }
        public int? Current { get; set; }
        public int? Total { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? EstimatedSecondsLeft { get; set; }
    }
}
