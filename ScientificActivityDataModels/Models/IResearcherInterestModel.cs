using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IResearcherInterestModel : IId
    {
        int ResearcherId { get; }
        string Keyword { get; }
        decimal Weight { get; }
    }
}
