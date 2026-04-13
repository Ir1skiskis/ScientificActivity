using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class GrantTag
    {
        public int Id { get; set; }

        [Required]
        public int GrantId { get; set; }

        [Required]
        public int TagId { get; set; }

        public virtual Grant Grant { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
