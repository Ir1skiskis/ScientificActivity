using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IImportService
    {
        Task<int> ImportGrantsAsync(CancellationToken cancellationToken = default);
        Task<int> ImportConferencesAsync(CancellationToken cancellationToken = default);
        Task<int> ImportVakJournalsAsync(string pdfPath, CancellationToken cancellationToken = default);
    }
}
