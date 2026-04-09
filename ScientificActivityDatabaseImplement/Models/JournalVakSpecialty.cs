using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class JournalVakSpecialty
    {
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }

        [Required]
        public string SpecialtyCode { get; set; } = string.Empty;

        [Required]
        public string SpecialtyName { get; set; } = string.Empty;

        [Required]
        public string ScienceBranch { get; set; } = string.Empty;

        [Required]
        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(JournalId))]
        public virtual Journal Journal { get; set; } = null!;

        public static JournalVakSpecialty? Create(JournalVakSpecialtyBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new JournalVakSpecialty
            {
                Id = model.Id,
                JournalId = model.JournalId,
                SpecialtyCode = model.SpecialtyCode,
                SpecialtyName = model.SpecialtyName,
                ScienceBranch = model.ScienceBranch,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo
            };
        }

        public JournalVakSpecialtyViewModel GetViewModel => new()
        {
            Id = Id,
            JournalId = JournalId,
            SpecialtyCode = SpecialtyCode,
            SpecialtyName = SpecialtyName,
            ScienceBranch = ScienceBranch,
            DateFrom = DateFrom,
            DateTo = DateTo
        };
    }
}
