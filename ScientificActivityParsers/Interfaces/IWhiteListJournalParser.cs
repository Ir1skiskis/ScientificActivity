using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IWhiteListJournalParser
    {
        Task<List<JournalImportModel>> ParseAsync(CancellationToken cancellationToken = default);
    }
}
