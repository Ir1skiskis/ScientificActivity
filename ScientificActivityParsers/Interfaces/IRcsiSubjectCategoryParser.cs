using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IRcsiSubjectCategoryParser
    {
        Task<List<string>> ParseCategoriesAsync(int? recordSourceId, string? journalUrl, CancellationToken cancellationToken = default);
    }
}
