using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class JournalTag
    {
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }

        [Required]
        public int TagId { get; set; }

        public virtual Journal Journal { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
