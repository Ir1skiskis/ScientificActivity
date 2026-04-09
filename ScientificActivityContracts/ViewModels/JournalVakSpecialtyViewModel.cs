using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class JournalVakSpecialtyViewModel
    {
        public int Id { get; set; }

        [DisplayName("Журнал")]
        public int JournalId { get; set; }

        [DisplayName("Код специальности")]
        public string SpecialtyCode { get; set; } = string.Empty;

        [DisplayName("Название специальности")]
        public string SpecialtyName { get; set; } = string.Empty;

        [DisplayName("Отрасль науки")]
        public string ScienceBranch { get; set; } = string.Empty;

        [DisplayName("Дата начала")]
        public DateTime DateFrom { get; set; }

        [DisplayName("Дата окончания")]
        public DateTime? DateTo { get; set; }
    }
}
