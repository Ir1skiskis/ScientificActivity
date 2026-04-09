using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class JournalPagedListViewModel
    {
        public List<JournalViewModel> Journals { get; set; } = new();

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public string? Title { get; set; }
        public string? Issn { get; set; }
        public bool? IsVak { get; set; }
        public bool? IsWhiteList { get; set; }
    }
}
