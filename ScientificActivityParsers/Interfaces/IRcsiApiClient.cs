using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IRcsiApiClient
    {
        Task<List<RcsiRecordSourceModel>> GetAllRecordSourcesAsync(CancellationToken cancellationToken = default);
    }
}
