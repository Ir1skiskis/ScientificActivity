using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class ConferenceTag
    {
        public int Id { get; set; }

        [Required]
        public int ConferenceId { get; set; }

        [Required]
        public int TagId { get; set; }

        public virtual Conference Conference { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
