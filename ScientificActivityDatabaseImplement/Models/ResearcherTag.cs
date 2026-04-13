using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class ResearcherTag
    {
        public int Id { get; set; }

        [Required]
        public int ResearcherId { get; set; }

        [Required]
        public int TagId { get; set; }

        public virtual Researcher Researcher { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
