using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class ParserProgressModel
    {
        public string StatusText { get; set; } = string.Empty;
        public int? Current { get; set; }
        public int? Total { get; set; }
        public int? Percent { get; set; }
    }
}
